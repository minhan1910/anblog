import { UtilityService } from 'src/app/shared/services/utility.service';
import { SystemConstants } from './../constants/system.constants';
import { Directive, ElementRef, Input, OnInit } from '@angular/core';
import { TokenStorageService } from './../services/token-storage.service';

@Directive({
  selector: '[appPermission]',
})
export class PermissionDirective implements OnInit {
  @Input() appPolicy: string;

  constructor(
    private el: ElementRef,
    private utilityService: UtilityService,
    private tokenService: TokenStorageService
  ) {}
  ngOnInit() {
    const loggedInUser = this.tokenService.getUser();
    const elementStyle = this.el.nativeElement.style;

    if (loggedInUser) {
      const listPermission = loggedInUser.permissions;

      const hasListPermission = !this.utilityService.isEmpty(listPermission);

      // const listPermisisonAppliedAppPolicy =
      //   hasListPermission &&
      //   listPermission.filter((x: any) => x == this.appPolicy).length > 0;

      const listPermisisonAppliedAppPolicy =
        hasListPermission && listPermission.includes(this.appPolicy);

      elementStyle.display = listPermisisonAppliedAppPolicy
        ? SystemConstants.DISPLAY_NONE
        : SystemConstants.STRING_EMPTY;
    } else {
      elementStyle.display = SystemConstants.DISPLAY_NONE;
    }
  }

  private isListPermissionNullOrEmpty(listPermission: string) {
    return (
      listPermission == null || listPermission == SystemConstants.STRING_EMPTY
    );
  }
}
