import { Injectable } from '@angular/core';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { Observable } from 'rxjs';
import { ConfirmDialogComponent } from '../_modals/confirm-dialog/confirm-dialog.component';

@Injectable({
  providedIn: 'root'
})
export class ConfirmService {
  bsModelRef: BsModalRef

  constructor(private modalService: BsModalService) { }

  confirm(title = 'Confirmation', message = 'Are you sure that you want to do this?', btnOkText = 'Ok', btnCancelText = 'Cancel') : Observable<boolean>{
    const config = {
      //anything in the initialState is available to the confirm-dialog component
      initialState: {
        title,
        message,
        btnOkText,
        btnCancelText
      }
    }
    this.bsModelRef = this.modalService.show(ConfirmDialogComponent, config);
    return new Observable<boolean>(this.getResult());
  }

  private getResult(){
    return (observer) => {
      const subscription = this.bsModelRef.onHidden.subscribe(() => {
        observer.next(this.bsModelRef.content.result);
        observer.complete()
      });
      return {
        unsubscribe(){
          subscription.unsubscribe();
        }
      }
    }
  }
}
