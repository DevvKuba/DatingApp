import { HttpClient} from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { Member } from '../_models/member';
import { of, tap } from 'rxjs';
import { Photo } from '../_models/photo';

@Injectable({
  providedIn: 'root'
})
export class MembersService {
  
  private http = inject(HttpClient);
  baseUrl = environment.apiUrl;

  //local cache
  members = signal<Member[]>([]);

  // gets members from the API and stores them in memory(in the signal);
  getMembers(){
    return this.http.get<Member[]>(this.baseUrl + 'users').subscribe({
      next: members => this.members.set(members) 
    })
  }

  getMember(username: string){
    const member = this.members().find(x => x.username === username);
    if(member !== undefined) return of(member);

    return this.http.get<Member>(this.baseUrl + 'users/' + username);
  }

  // calls put request from our api, that updates member data in database
  updateMember(member: Member){
    return this.http.put(this.baseUrl + 'users', member).pipe(
      tap(() => {
        this.members.update(members => members.map(m => m.username === member.username ? member : m))
      })
    )
  }


  setMainPhoto(photo: Photo){
    return this.http.put(this.baseUrl + 'users/set-main-photo/' + photo.id, {})
    // below functionality updates the members array , specifically m or member making sure it includes the photo
    // m.photoUrl being equal to the passed photo.url, clicked photo
    .pipe(
      tap(() => {
        this.members.update(members => members.map(m => {
          if(m.photos.includes(photo)){
            m.photoUrl = photo.url
          }
          return m;
        }))
      })
    )
  }

  deletePhoto(photo: Photo){
    return this.http.delete(this.baseUrl + 'users/delete-photo/' + photo.id).pipe(
      tap(() => {
        this.members.update(members => members.map(m => {
          // if members arr still includes photo we deleted filter it from the array
          if(m.photos.includes(photo)){
            m.photos = m.photos.filter(x => x.id !== photo.id)
          }
          return m;
        }))
      })
    );
  }

  
}
