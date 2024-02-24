import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';

// primeng
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { BlockUIModule } from 'primeng/blockui';
import { PaginatorModule } from 'primeng/paginator';
import { PanelModule } from 'primeng/panel';
import { CheckboxModule } from 'primeng/checkbox';
import { SharedModule } from 'primeng/api';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { KeyFilterModule } from 'primeng/keyfilter';
import { BadgeModule } from 'primeng/badge';
import { PickListModule } from 'primeng/picklist';
import { ImageModule } from 'primeng/image';

// Other modules
import { SystemRoutingModule } from './system-routing.module';

// Components
import { UserComponent } from './users/user.component';
import { RoleComponent } from './roles/role.component';
import { PermissionGrantComponent } from './roles/permission-grant.component';
import { AnSharedModule } from './../../shared/modules/an-shared.module';
import { RolesDetailComponent } from './roles/role-detail.component';
import { UserDetailComponent } from './users/user-detail.component';
import { SetPasswordComponent } from './users/set-password.component';
import { RoleAssignComponent } from './users/role-assign.component';
import { ChangeEmailComponent } from './users/change-email.component';

@NgModule({
  imports: [
    SystemRoutingModule,
    CommonModule,
    ReactiveFormsModule,
    TableModule,
    ProgressSpinnerModule,
    BlockUIModule,
    PaginatorModule,
    PanelModule,
    CheckboxModule,
    ButtonModule,
    InputTextModule,
    SharedModule,
    AnSharedModule,
    KeyFilterModule,
    BadgeModule,
    PickListModule,
    ImageModule,
  ],
  declarations: [
    UserComponent,
    UserDetailComponent,
    SetPasswordComponent,
    RoleAssignComponent,
    RoleComponent,
    RolesDetailComponent,
    PermissionGrantComponent,
    ChangeEmailComponent,
  ],
})
export class SystemModule {}
