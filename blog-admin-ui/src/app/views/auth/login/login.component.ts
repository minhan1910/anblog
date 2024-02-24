import { SystemConstants } from './../../../shared/constants/system.constants';
import { TokenStorageService } from './../../../shared/services/token-storage.service';
import { UrlConstants } from './../../../shared/constants/url.constants';
import { AlertService } from './../../../shared/services/alert.service';
import { Component, OnDestroy } from '@angular/core';
import {
  FormBuilder,
  FormControl,
  FormGroup,
  Validators,
} from '@angular/forms';
import { Router } from '@angular/router';
import {
  AdminApiAuthApiClient,
  AuthenticatedResult,
  LoginRequest,
} from 'src/app/api/admin-api.service.generated';
import { Subject, takeUntil } from 'rxjs';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
})
export class LoginComponent implements OnDestroy {
  loginForm: FormGroup;
  loading = false;

  private ngUnsubrible = new Subject<void>();

  constructor(
    private readonly formBuilder: FormBuilder,
    private readonly authApiClient: AdminApiAuthApiClient,
    private readonly alertService: AlertService,
    private readonly router: Router,
    private readonly tokenStorageService: TokenStorageService
  ) {
    this.loginForm = this.formBuilder.group({
      username: new FormControl(null, Validators.required),
      password: new FormControl(null, Validators.required),
    });
  }

  ngOnDestroy(): void {
    this.ngUnsubrible.next();
    this.ngUnsubrible.complete();
  }

  login() {
    this.loading = true;

    const request: LoginRequest = new LoginRequest({
      username: this.loginForm.controls['username'].value,
      password: this.loginForm.controls['password'].value,
    });

    this.authApiClient
      .login(request)
      .pipe(takeUntil(this.ngUnsubrible))
      .subscribe({
        next: (response: AuthenticatedResult) => {
          // Save token and refresh token to localStorage

          this.loading = false;
          
          this.tokenStorageService.saveToken(
            response.token ?? SystemConstants.STRING_EMPTY
          );

          this.tokenStorageService.saveRefreshToken(
            response.refreshToken ?? SystemConstants.STRING_EMPTY
          );

          this.tokenStorageService.saveUser(response);

          this.alertService.showSuccess(`Login Successfully!`);

          this.router.navigate([UrlConstants.HOME]);
        },
        error: (error: any) => {
          console.log(error);
          this.loading = false;
          this.alertService.showError(`Login Failed!`);
        },
      });
  }
}
