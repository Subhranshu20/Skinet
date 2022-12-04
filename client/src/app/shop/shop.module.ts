import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ShopComponent } from './shop.component';
import { ProductItemComponent } from "./product-item/product-item.component";
import { SharedModule } from '../shared/shared.module';
import { ProductDetailsComponent } from './product-details/product-details.component';
import { RouterModule } from '@angular/router';
import { ShopRoutineModule } from './shop-routine.module';



@NgModule({
    declarations: [
        ShopComponent,
        ProductItemComponent,
        ProductDetailsComponent
    ],
    exports: [//ShopComponent,
    ProductItemComponent
    ],
    imports: [
        CommonModule,
        SharedModule,
       ShopRoutineModule
    ]
})
export class ShopModule { }
