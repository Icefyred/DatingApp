import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { User } from '../_models/user';
import { AccountService } from '../_services/account.service';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {
  model : any = {}
  loggedIn : boolean;
  //currentUser$: Observable<User>;

  constructor(public accountService: AccountService) { }

  ngOnInit(): void {
    //this.getCurrentUser();
    //this.currentUser$ = this.accountService.currentUser$;
  }

  login(){
    this.accountService.login(this.model).subscribe(response => {
      console.log(response);
      //this.loggedIn = true;
    }, error =>{
      console.log(error);
    })
  }

  logout(){
    this.accountService.logout();
    //this.loggedIn = false;
  }

  /*
  getCurrentUser(){
    //this currentUser is not an http request, so this never completes and will cause memory leaks
    //so even if it works it's not good practice
    this.accountService.currentUser$.subscribe(user => {
      //the double '!' turn the object into boolean
      this.loggedIn = !!user;
    }, error => {
      console.log(error);
    })
  }
  */

}
