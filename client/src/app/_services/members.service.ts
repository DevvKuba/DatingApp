import { HttpClient, HttpParams, HttpResponse} from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { Member } from '../_models/member';
import { of, tap } from 'rxjs';
import { Photo } from '../_models/photo';
import { PaginatedResult } from '../_models/pagination';
import { UserParams } from '../_models/userParams';
import { observeNotification } from 'rxjs/internal/Notification';
import { AccountService } from './account.service';

@Injectable({
  providedIn: 'root'
})
export class MembersService {
  private http = inject(HttpClient);
  private accountService = inject(AccountService);
  baseUrl = environment.apiUrl;
  paginatedResult = signal<PaginatedResult<Member[]> | null>(null);
  memberCache = new Map();
  user = this.accountService.currentUser();
  userParams = signal<UserParams>(new UserParams(this.user));

  resetUserParams(){
    this.userParams.set(new UserParams(this.user));
  }

  // gets members from the API and stores them in memory(in the signal);
  getMembers(){
    const response = this.memberCache.get(Object.values(this.userParams()).join('-'));

    if(response) return this.setPaginatedResponse(response);

    let params = this.setPaginationHeaders(this.userParams().pageNumber, this.userParams().pageSize);

    params = params.append('minAge', this.userParams().minAge);
    params = params.append('maxAge', this.userParams().maxAge);
    params = params.append('gender', this.userParams().gender);
    params = params.append('orderBy', this.userParams().orderBy);

    return this.http.get<Member[]>(this.baseUrl + 'users', {observe: 'response', params}).subscribe({
      next: response => {
        this.setPaginatedResponse(response);
        this.memberCache.set(Object.values(this.userParams()).join('-'), response);
      } 
    })
  }

  private setPaginatedResponse(response: HttpResponse<Member[]>){
    this.paginatedResult.set({
          items: response.body as Member[],
          pagination: JSON.parse(response.headers.get('Pagination')!)
        })
  }

  private setPaginationHeaders(pageNumber: number, pageSize: number){
    let params = new HttpParams();

    if(pageNumber && pageSize){
      params = params.append('pageNumber', pageNumber);
      params = params.append('pageSize', pageSize);
    }

    return params;
  }

  getMember(username: string){
    // adds all members on given page to memberCache
    const member: Member = [...this.memberCache.values()]
    .reduce((arr, elem) => arr.concat(elem.body), [])
    .find((m:Member) => m.username === username);

    if(member) return of(member);


    return this.http.get<Member>(this.baseUrl + 'users/' + username);
  }

  // calls put request from our api, that updates member data in database
  updateMember(member: Member){
    return this.http.put(this.baseUrl + 'users', member).pipe(
      // tap(() => {
      //   this.members.update(members => members.map(m => m.username === member.username ? member : m))
      // })
    )
  }


  setMainPhoto(photo: Photo){
    return this.http.put(this.baseUrl + 'users/set-main-photo/' + photo.id, {})
    // below functionality updates the members array , specifically m or member making sure it includes the photo
    // m.photoUrl being equal to the passed photo.url, clicked photo
    .pipe(
      // tap(() => {
      //   this.members.update(members => members.map(m => {
      //     if(m.photos.includes(photo)){
      //       m.photoUrl = photo.url
      //     }
      //     return m;
      //   }))
      // })
    )
  }

  deletePhoto(photo: Photo){
    return this.http.delete(this.baseUrl + 'users/delete-photo/' + photo.id).pipe(
      // tap(() => {
      //   this.members.update(members => members.map(m => {
      //     // if members arr still includes photo we deleted filter it from the array
      //     if(m.photos.includes(photo)){
      //       m.photos = m.photos.filter(x => x.id !== photo.id)
      //     }
      //     return m;
      //   }))
      // })
    );
  }

  
}
