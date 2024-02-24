import { TokenStorageService } from './../../../shared/services/token-storage.service';
import { Component, Input } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { Router } from '@angular/router';

import { ClassToggleService, HeaderComponent } from '@coreui/angular';
import { UrlConstants } from 'src/app/shared/constants/url.constants';

@Component({
  selector: 'app-default-header',
  templateUrl: './default-header.component.html',
})
export class DefaultHeaderComponent extends HeaderComponent {

  @Input() sidebarId: string = "sidebar";

  public newMessages = new Array(4)
  public newTasks = new Array(5)
  public newNotifications = new Array(5)

  constructor(private classToggler: ClassToggleService,
     private readonly router: Router,
     private readonly tokenStorageService: TokenStorageService) {
    super();
  }

  logout() {
    this.tokenStorageService.signOut();
    this.router.navigate([UrlConstants.LOGIN]);
  }
}
