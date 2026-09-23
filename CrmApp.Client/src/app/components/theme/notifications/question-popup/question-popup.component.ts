import { Component, inject } from '@angular/core';
import { MAT_SNACK_BAR_DATA, MatSnackBarRef } from '@angular/material/snack-bar';
import { MessagePopupData } from '../message-popup/message-popup.component';

export interface QuestionPopupData {
  message: string;
  type: 'question';
  icon: string;
  data?: any;
}
@Component({
  selector: 'crm-question-popup',
  standalone: false,
  templateUrl: './question-popup.component.html',
  styleUrl: './question-popup.component.css'
})
export class QuestionPopupComponent {
  snackBarRef = inject(MatSnackBarRef);
  data = inject(MAT_SNACK_BAR_DATA) as QuestionPopupData;
  

  close(isAccept: boolean) {
    isAccept ? this.snackBarRef.dismissWithAction() : this.snackBarRef.dismiss();
  }
}
