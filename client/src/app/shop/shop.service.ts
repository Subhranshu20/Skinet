import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map } from 'rxjs/operators';
import { IBrand } from '../shared/models/brand';
import { IPagination } from '../shared/models/pagination';
import { IType } from '../shared/models/productType';
import { ShopParams } from '../shared/models/shopParams';

@Injectable({
  providedIn: 'root'
})
export class ShopService {
  baseUrl = 'https://localhost:7240/api/';

  constructor(private http: HttpClient) { }

  getProducts(shopParams:ShopParams){
    let params = new HttpParams();
    if(shopParams.brandId){
      params = params.append('brandId',shopParams.brandId.toString());
    }
    if(shopParams.typeId){
      params = params.append('typeId',shopParams.typeId.toString());
    }
    // if(shopParams.sort){ 
      
    // }
    // if(shopParams.pageNumber){ 
      
    // }
    if(shopParams.search){ 
        params=params.append('Search',shopParams.search);
       }
    params = params.append('sort',shopParams.sort);
    params = params.append('pageIndex',shopParams.pageNumber);
    params = params.append('pageIndex',shopParams.pageSize.toString());
    return this.http.get<IPagination>(this.baseUrl + 'products',{observe: 'response', params})
      .pipe(
        map(response => {
          return response.body;
        })
      );
  }
  getBrands()
  {
    return this.http.get<IBrand[]>(this.baseUrl + 'products/brands')
  }
  getTypes()
  {
    return this.http.get<IType[]>(this.baseUrl + 'products/types')
  }
  

}
