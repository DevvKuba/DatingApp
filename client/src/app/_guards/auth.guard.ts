import { CanActivateFn } from '@angular/router';
import { AccountService } from '../_services/account.service';
import { inject } from '@angular/core';
import { ToastrService } from 'ngx-toastr';

// further declare which 'files' you want to protect with the route guard

export const authGuard: CanActivateFn = (route, state) => {
  const accountService = inject(AccountService);
  const toastr = inject(ToastrService);

  if(accountService.currentUser()){
    return true;
  } else {
    toastr.error('You shall not pass!');
    return false;
  }
};
