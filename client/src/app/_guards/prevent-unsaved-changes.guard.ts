import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanDeactivate, RouterStateSnapshot, UrlTree } from '@angular/router';
import { Observable } from 'rxjs';
import { MemberEditComponent } from '../members/member-edit/member-edit.component';
import { ConfirmService } from '../_services/confirm.service';

@Injectable({
  providedIn: 'root'
})
export class PreventUnsavedChangesGuard implements CanDeactivate<unknown> {

  constructor(private confirmService: ConfirmService){}

  canDeactivate(
    //the term for the '|' pipe for the possible of two different return type is an union type,
    //meaning it can return either an Observable of type boolean, or just simply a boolean
    component: MemberEditComponent): Observable<boolean> | boolean {
      if(component.editForm.dirty){
        return this.confirmService.confirm();
      }
      return true;
  }
  
}
