import { Component, OnInit } from '@angular/core';
import { Photo } from 'src/app/_models/photo';
import { AdminService } from 'src/app/_services/admin.service';

@Component({
  selector: 'app-photo-management',
  templateUrl: './photo-management.component.html',
  styleUrls: ['./photo-management.component.css']
})
export class PhotoManagementComponent implements OnInit {
  photos: Photo[];

  constructor(private adminService: AdminService) { }

  ngOnInit(): void {
    this.getPhotosForApproval();
  }

  /****** Phtoto Management Challenge 18. ******/

  getPhotosForApproval(){
    this.adminService.getPhotosForApproval().subscribe(photo => {
      this.photos = photo;
    })
  }

  approvePhoto(id: number){
    this.adminService.approvePhoto(id).subscribe(() =>{
      this.photos.splice(this.photos.findIndex(photo => photo.id === id), 1);
    })
  }

  rejectPhoto(id: number){
    this.adminService.rejectPhoto(id).subscribe(() => {
      this.photos.splice(this.photos.findIndex(photo => photo.id === id), 1);
    })
  }

}
