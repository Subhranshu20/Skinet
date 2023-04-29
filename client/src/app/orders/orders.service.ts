import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Order } from '../shared/models/order';

@Injectable({
  providedIn: 'root'
})
export class OrdersService {
  baseUrl = 'https://localhost:7240/api/';
 
  constructor(private http: HttpClient) { }

  getOrdersForUser() {
    
      var result = this.http.get<Order[]>(this.baseUrl + 'orders');
      console.log(result);
      return result;
  }
  getOrderDetailed(id: number) {
      return this.http.get<Order>(this.baseUrl + 'orders/' + id);
  }

}
