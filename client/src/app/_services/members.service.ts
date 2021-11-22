import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { of } from 'rxjs';
import { map, take } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { Member } from '../_models/member';
import { PaginatedResult } from '../_models/pagination';
import { User } from '../_models/user';
import { UserParams } from '../_models/userParams';
import { AccountService } from './account.service';
import { getPaginatedResult, getPaginationHeaders } from './paginationHelper';

@Injectable({
  providedIn: 'root'
})
export class MembersService {
  baseUrl = environment.apiUrl;
  //array that will keep the members array to keep the state of a request, 
  //so the loading won't happen over and over again when opening the same page
  members: Member[] = [];
  //a good thing to use for caching is a map, it works similarly as a dictionary, with a key and value
  memberCache = new Map();
  user: User;
  userParams: UserParams;
  

  constructor(private http: HttpClient, private accountService: AccountService) {
    this.accountService.currentUser$.pipe(take(1)).subscribe(user => {
      this.user = user;
      this.userParams = new UserParams(user);
    })
   }

  getUserParams(){
    return this.userParams
  }

  setUserParams(params: UserParams){
    this.userParams = params;
  }
  resetUserParams(){
    this.userParams = new UserParams(this.user);
    return this.userParams;
  }

  getMembers(userParams: UserParams){
    var response = this.memberCache.get(Object.values(userParams).join('-'));
    if(response){
      return of(response);
    }
    
    //console.log(Object.values(userParams).join('-'));
    let params = getPaginationHeaders(userParams.pageNumber, userParams.pageSize);

    params = params.append('minAge', userParams.minAge.toString());
    params = params.append('maxAge', userParams.maxAge.toString());
    params = params.append('gender', userParams.gender);
    params = params.append('orderBy', userParams.orderBy);

    //the of() method means that it will return something of an Observable
    //if(this.members.length > 0) return of(this.members);
    //before when we were using http normally this would have given us the response body,
    //when we're observing the response then we get the full response back
    return getPaginatedResult<Member[]>(this.baseUrl + 'users', params, this.http)
      .pipe(map(response => {
        this.memberCache.set(Object.values(userParams).join('-'), response);
        return response;
      }));
  }

  

  getMember(username: string){
    //the ... means a spread operator, it receieves and iterable and expands it into individual elements
    const member = [...this.memberCache.values()]
      //arr for previous array, and elem for next value
      .reduce((arr, elem) => arr.concat(elem.result), [])
      .find((member: Member) => member.username === username);
    
    if(member){
      return of(member);
    }

    //const member = this.members.find(x => x.username === username);
    //if(member !== undefined) return of(member);
    return this.http.get<Member>(this.baseUrl + 'users/' + username);
  }

  updateMember(member: Member){
    return this.http.put(this.baseUrl + 'users', member).pipe(
      map(() => {
        const index = this.members.indexOf(member);
        this.members[index] = member;
      })
    );
  }

  setMainPhoto(photoId: number){
    return this.http.put(this.baseUrl + 'users/set-main-photo/' + photoId, {});
  }

  deletePhoto(photoId: number){
    return this.http.delete(this.baseUrl + 'users/delete-photo/' + photoId);
  }

  addLike(username: string){
    return this.http.post(this.baseUrl + 'likes/' + username, {});
  }

  getLikes(predicate: string, pageNumber, pageSize){
    let params = getPaginationHeaders(pageNumber, pageSize);
    params = params.append('predicate', predicate);
    //return this.http.get<Partial<Member[]>>(this.baseUrl + 'likes?predicate=' + predicate);
    return getPaginatedResult<Partial<Member[]>>(this.baseUrl + 'likes', params, this.http);
  }
}
