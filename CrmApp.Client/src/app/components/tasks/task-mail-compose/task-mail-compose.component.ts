import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { environment } from 'src/environments/environment';
import { SendMailDTO } from 'src/models/DTO/SendMailDTO';

@Component({
  selector: 'crm-task-mail-compose',
  standalone: false,
  templateUrl: './task-mail-compose.component.html',
  styleUrl: './task-mail-compose.component.css',
})
export class TaskMailComposeComponent implements OnInit {
  @Input() toEmail!: string;
  @Input() toName!: string;
  @Input() taskTitle!: string;
  @Input() taskNumber!: string;
  @Input() taskId?: number;
 
  @Output() sent = new EventEmitter<void>();
  @Output() cancelled = new EventEmitter<void>();
 
  form!: FormGroup;
  sending = false;
  error = '';
 
  constructor(private fb: FormBuilder
  ) {}
 
  ngOnInit(): void {
    this.form = this.fb.group({
      subject: [`RE: ${this.taskTitle} [${this.taskNumber}]`, Validators.required],
      body: ['', Validators.required],
    });
  }
 
  send(): void {
    if (this.form.invalid) return;
    this.sending = true;
    this.error = '';
 
    const dto: SendMailDTO = {
      to: this.toEmail,
      toName: this.toName,
      subject: this.form.value.subject,
      body: this.form.value.body,
      taskId: this.taskId,
    };
    // TO DO 
  }
}