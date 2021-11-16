import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ReplaySubject } from 'rxjs';
import {map} from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { User } from '../_models/user';

//services are injectable
@Injectable({
  providedIn: 'root'
})
//services are singleton, the data that is stored inside the server
//doesn't get destroyed until the application is closed down

//components are different, when we move from one component to another in angular are destroyed,
//as soon as they're not used
export class AccountService {
  //this is to avoid hardcoding the URL string, by using the environment files
  baseUrl = environment.apiUrl;

  //observable to store the user
  //ReplaySubject is a buffer object, it will store the values
  //and any time a subscriber subscribes it will emit the last value inside
  //or how many we want to emit
  private currentUserSource = new ReplaySubject<User>(1);
  currentUser$ = this.currentUserSource.asObservable();

  constructor(private http: HttpClient) { }

  login(model: any){
    return this.http.post(this.baseUrl + 'account/login', model).pipe(
      map((response: User) => {
        const user = response;
        if(user){
          localStorage.setItem('user', JSON.stringify(user));
          this.currentUserSource.next(user);
        }
      })
    );
  }

  register(model: any){
    return this.http.post(this.baseUrl + 'account/register', model).pipe(
      map((user: User) => {
        if(user){
          localStorage.setItem('user', JSON.stringify(user));
          this.currentUserSource.next(user);  
        }
        //this is so that it won't appear undefined in the response that comes 
        //from the register.component.ts in the browser console
        //return user;
      })
    )
  }

  setCurrentUser(user: User){
    this.currentUserSource.next(user);
  }

  logout(){
    localStorage.removeItem('user');
    this.currentUserSource.next(null);
  }
}
