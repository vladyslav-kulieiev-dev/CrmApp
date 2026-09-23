import { Component, inject, OnInit } from '@angular/core';
import { Form, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { AuthService } from 'src/app/core/services/auth.service';
import { NotificationService } from 'src/app/core/services/notification.service';
import { LoginDTO } from 'src/models/DTO/LoginDTO';

@Component({
    selector: 'crm-login-view',
    templateUrl: './login-view.component.html',
    styleUrls: ['./login-view.component.css'],
    standalone: false
})
export class LoginViewComponent {
  private _snackBar = inject(MatSnackBar);

  hide = true;
  form!: FormGroup;
  loading: boolean = false;
  error: string | null = null;

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private authService: AuthService,
    private notification: NotificationService
  ) {
    this.form = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      rememberMe: [true, [Validators.required]]
    });
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    var loginDTO: LoginDTO = {
      email: this.form.value.email,
      password: this.form.value.password,
      rememberMe: this.form.value.rememberMe, // TODO: implement remember me functionality
      lockoutOnFailure: false // TODO: implement lockout on failure
    };
    this.authService.login(loginDTO).subscribe((res) => {
      this.loading = false;
      if (res.succeeded) { 
        this.notification.success("Zalogowano!");
        this.loading = false; 
        this.router.navigateByUrl('/'); 
      }
      else {
        this.error = res.errors?.length ? res.errors[0] : 'Logowanie nie powiodło się';
        this.form.markAllAsTouched();
        this.notification.error(this.error);
      }
    }, (err) => {
      this.loading = false;
      this.error = 'Logowanie nie powiodło się';
      this.form.markAllAsTouched();
      console.error('Login error:', err);
      this.notification.error(this.error);
    });
  }

  get email() { return this.form.get('email'); }
  get password() { return this.form.get('password'); }
}
