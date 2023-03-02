import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, findIndex, map } from 'rxjs';
import {  Basket, BasketTotals, IBasket, IBasketItem } from '../shared/models/basket';
import { IProduct } from '../shared/models/product';

@Injectable({
  providedIn: 'root'
})
export class BasketService {
  baseUrl = 'https://localhost:7240/api/';
  private basketSource = new BehaviorSubject<IBasket>(null);
  basketSource$= this.basketSource.asObservable();
  private basketTotalSource = new BehaviorSubject<BasketTotals | null>(null);
  basketTotalSource$= this.basketTotalSource.asObservable();
  constructor(private http: HttpClient) { }

  getBasket(id: string)
  {
    // return this.http.get(this.baseUrl + 'basket?id='+id)
    // .pipe(
    //   map((basket: IBasket)=> {
    //     this.basketSource.next(basket);
    //   })
    // )
    return this.http.get<IBasket>(this.baseUrl + 'basket?id='+id).subscribe({
      next: basket => {
        this.basketSource.next(basket);
        this.calculateTotals();
      }
    })
  }
  setBasket(basket: IBasket)
  {
    return this.http.post<IBasket>(this.baseUrl + 'basket', basket).subscribe({
      next: basket => {
        this.basketSource.next(basket);
        this.calculateTotals();
      }
    })

    // .subscribe((response: IBasket) =>{
    //   this.basketSource.next(response);
    //   console.log(response);
    // },error=>{
    //   console.log(error);
    // });
  }
  getCurrentBasketValue(){
    return this.basketSource.value;
  }

  // addItemToBasket(item: IProduct | IBasketItem, quantity=1){
    
  //   if (this.isProduct(item)) item = this.mapProductItemToBasketItem(item);
  //   //const itemToAdd: IBasketItem =this.mapProductItemToBasketItem(item,quantity);
  //   const basket = this.getCurrentBasketValue() ?? this.createBasket();
  //   //basket.items.push(itemToAdd);
  //   basket.items=this.addOrUpdateItem(basket.items,item,quantity);
  //   this.setBasket(basket);
  // }
  addItemToBasket(item: IProduct | IBasketItem, quantity = 1) {
    if (this.isProduct(item)) item = this.mapProductItemToBasketItem(item);    
    const basket = this.getCurrentBasketValue() ?? this.createBasket();
    basket.items = this.addOrUpdateItem(basket.items, item, quantity);
    this.setBasket(basket);
  }

  // removeItemFromBasket(id: number, quantity = 1) {
  //   const basket = this.getCurrentBasketValue();
  //   if (!basket) return;
  //   const item = basket.items.find(x => x.id === id);
  //   if (item) {
  //     item.quantity -= quantity;
  //     if (item.quantity === 0) {
  //       basket.items = basket.items.filter(x => x.id !== id);
  //     }
  //     if (basket.items.length > 0) this.setBasket(basket);
  //     else this.deleteBasket(basket);
  //   }
  // }
  removeItemFromBasket(id: number, quantity = 1) {
    const basket = this.getCurrentBasketValue();
    if (!basket) return;
    const item = basket.items.find(x => x.id === id);
    if (item) {
      item.quantity -= quantity;
      if (item.quantity === 0) {
        basket.items = basket.items.filter(x => x.id !== id);
      }
      if (basket.items.length > 0) this.setBasket(basket);
      else this.deleteBasket(basket);
    }
  }
  
  deleteBasket(basket: Basket) {
    return this.http.delete(this.baseUrl + 'basket?id=' + basket.id).subscribe({
      next: () => {
        this.deleteLocalBasket();
      }
    })
  }

  deleteLocalBasket() {
    this.basketSource.next(null);
    this.basketTotalSource.next(null);
    localStorage.removeItem('basket_id');
  }
  // addOrUpdateItem(items: IBasketItem[], itemToAdd: IBasketItem, quantity: number): IBasketItem[] {
  //   console.log(items);
  //   const index = items.findIndex(i=> i.id===itemToAdd.id);
  //   if(index===-1)
  //   {
  //     itemToAdd.quantity=quantity;
  //     items.push(itemToAdd);
  //   }
  //   else{
  //     items[index].quantity+=quantity;
  //   }
  //   return items;

  // }
  private addOrUpdateItem(items: IBasketItem[], itemToAdd: IBasketItem, quantity: number): IBasketItem[] {
    const item = items.find(x => x.id === itemToAdd.id);
    if (item) item.quantity += quantity;
    else {
      itemToAdd.quantity = quantity;
      items.push(itemToAdd);
    }
    return items;
  }

  private createBasket(): IBasket {
    const basket = new Basket();
    localStorage.setItem('basket_Id',basket.id);
    return basket;
  }

  // private mapProductItemToBasketItem(item: IProduct , quantity: number): IBasketItem {
  //   return {
  //   id: item.id,
  //   productName:item.name,
  //   price: item.price,
  //   pictureUrl: item.pictureUrl,
  //   quantity,
  //   brand:item.productBrand,
  //   type:item.productType
  //   }

  // }
  private mapProductItemToBasketItem(item: IProduct): IBasketItem {
    return {
      id: item.id,
      productName: item.name,
      price: item.price,
      quantity: 0,
      pictureUrl: item.pictureUrl,
      brand: item.productBrand,
      type: item.productType
    }
  }

  private calculateTotals()
  {
    const basket = this.getCurrentBasketValue();
    if(!basket) return;
    const shipping=0;
    const subtotal = basket.items.reduce((a,b) => (b.price * b.quantity) + a,0);
    const total = subtotal + shipping;
    this.basketTotalSource.next({shipping, total, subtotal});
  }

  private isProduct(item: IProduct | IBasketItem): item is IProduct {
    return (item as IProduct).productBrand !== undefined;
  }

}
