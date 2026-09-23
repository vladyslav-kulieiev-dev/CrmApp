import { NumberSymbol } from '@angular/common';
import { Component, inject, Input } from '@angular/core';
import { MAT_SNACK_BAR_DATA, MatSnackBar, MatSnackBarHorizontalPosition, MatSnackBarRef, MatSnackBarVerticalPosition } from '@angular/material/snack-bar';


export interface MessagePopupData {
  message: string;
  type: 'info' | 'success' | 'error' | 'warning';
  icon: string;
  data?: any;
}

@Component({
  selector: 'crm-message-popup',
  standalone: false,
  templateUrl: './message-popup.component.html',
  styleUrl: './message-popup.component.css'
})
export class MessagePopupComponent {
  snackBarRef = inject(MatSnackBarRef);
  data = inject(MAT_SNACK_BAR_DATA) as MessagePopupData;

  constructor() {
  }
}
