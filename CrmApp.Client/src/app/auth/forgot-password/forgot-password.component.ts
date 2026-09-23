import { Component } from '@angular/core';
import { Validators, FormBuilder, FormGroup } from '@angular/forms';
import { AuthService } from 'src/app/core/services/auth.service';

@Component({
  selector: 'crm-forgot-password',
  standalone: false,
  templateUrl: './forgot-password.component.html',
  styleUrl: './forgot-password.component.css'
})
export class ForgotPasswordComponent {
  sending = false;
  success = false;
  form: FormGroup;

  constructor(private fb: FormBuilder, private auth: AuthService) {
    this.form = this.fb.group({
      email: ['', [Validators.required, Validators.email]]
    });
  }

  get email() { return this.form.get('email'); }

  submit() {
    if (this.form.invalid) return;
    this.sending = true;
    this.auth.forgotPassword(this.form.value.email!)
      .subscribe({
        next: () => { this.success = true; this.sending = false; },
        error: () => { this.success = true; this.sending = false; } // same response to avoid user enumeration
      });
  }
}
