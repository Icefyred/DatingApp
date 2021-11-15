import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
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

  constructor(public accountService: AccountService, private router: Router, private toastr: ToastrService) { }

  ngOnInit(): void {
    //this.getCurrentUser();
    //this.currentUser$ = this.accountService.currentUser$;
  }

  login(){
    this.accountService.login(this.model).subscribe(response => {
      this.router.navigateByUrl('/members');
      //console.log(response);
      //this.loggedIn = true;
    }, error =>{
      console.log(error);
      this.toastr.error(error.error);
    })
  }

  logout(){
    this.accountService.logout();
    this.router.navigateByUrl('/');
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
