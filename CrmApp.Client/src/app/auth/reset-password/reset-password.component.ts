import { Component, OnInit } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from 'src/app/core/services/auth.service';

function match(control: AbstractControl) {
  const p = control.get('password')?.value;
  const c = control.get('confirm')?.value;
  return p && c && p === c ? null : { mismatch: true };
}

@Component({
  selector: 'crm-reset-password',
  standalone: false,
  templateUrl: './reset-password.component.html',
  styleUrl: './reset-password.component.css'
})
export class ResetPasswordComponent implements OnInit {
  email = '';
  token = '';
  done = false;
  hide = true;
  form: FormGroup;

  constructor(private fb: FormBuilder, private route: ActivatedRoute, private auth: AuthService, private router: Router) { 
    this.form = this.fb.group({
      password: ['', [Validators.required, Validators.minLength(8)]],
      confirm: ['', Validators.required]
    }, { validators: match });
  }

  ngOnInit(): void {
    this.email = this.route.snapshot.queryParamMap.get('email') || '';
    this.token = this.route.snapshot.queryParamMap.get('token') || '';
  }

  get confirm() { return this.form.get('confirm'); }
  get password() { return this.form.get('password'); }

  submit() {
    if (this.form.invalid) return;
    this.auth.resetPassword(this.email, this.token, this.form.value.password!)
      .subscribe({
        next: () => { this.done = true; setTimeout(() => this.navigateToLogin(), 1500); },
        error: (err) => alert((err?.error?.errors || [err?.error?.message || 'Error']).join('\n'))
      });
  }

  async navigateToLogin() { 
    await this.auth.logout();
  }
}