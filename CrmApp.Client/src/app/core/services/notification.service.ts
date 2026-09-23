import { inject, Injectable, Injector, runInInjectionContext } from "@angular/core";
import { MatSnackBar, MatSnackBarConfig, MatSnackBarHorizontalPosition, MatSnackBarVerticalPosition } from "@angular/material/snack-bar";
import { MessagePopupComponent } from "../../components/theme/notifications/message-popup/message-popup.component";
import { QuestionPopupComponent } from "../../components/theme/notifications/question-popup/question-popup.component";
import { catchError, map, Observable, tap } from "rxjs";

type MessageType = 'info' | 'success' | 'error' | 'warning';
export const defaultSnackBarConfig: MatSnackBarConfig = {   
    duration: undefined,//5000,
    horizontalPosition: 'right',
    verticalPosition: 'top'
};
export const topCenterSnackBarConfig: MatSnackBarConfig = {   
    duration: undefined,//5000,
    horizontalPosition: 'center',
    verticalPosition: 'top'
};

export const defaultQuestionSnackBarConfig: MatSnackBarConfig = {   
    duration: undefined,
    horizontalPosition: 'right',
    verticalPosition: 'top'
};

function toArr(v?: string | string[]) {
  return v == null ? [] : Array.isArray(v) ? v : [v];
}

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private snackBar = inject(MatSnackBar);

    info(message: string, config: MatSnackBarConfig = defaultSnackBarConfig) {
    this.open('info', message, config);
  }

  success(message: string, config: MatSnackBarConfig = defaultSnackBarConfig) {
    this.open('success', message, config);
  }

  error(message: string, config: MatSnackBarConfig = defaultSnackBarConfig) {
    this.open('error', message, config);
  }

  warning(message: string, config: MatSnackBarConfig = defaultSnackBarConfig) {
    this.open('warning', message, config);
  }

  confirm(question: string, config: MatSnackBarConfig = defaultQuestionSnackBarConfig) {
    return this.openConfirmDialog(question, config);
  }

  private open(type: MessageType, message: string, config: MatSnackBarConfig) {
    const merged: MatSnackBarConfig = {
      ...defaultSnackBarConfig,
      ...config,
      panelClass: [
        ...toArr(defaultSnackBarConfig.panelClass),
        ...toArr(config.panelClass),
        `${type}-snackbar`,
      ],
      data: {
        ...(config.data ?? {}),
        message,
        type,
        icon: type
      },
    };

    this.snackBar.openFromComponent(MessagePopupComponent, merged);
  }

  private openConfirmDialog(message: string, config: MatSnackBarConfig): Observable<boolean> {
    const merged: MatSnackBarConfig = {
      ...defaultSnackBarConfig,
      ...config,
      panelClass: [
        ...toArr(defaultSnackBarConfig.panelClass),
        ...toArr(config.panelClass),
        `question-snackbar`,
      ],
      data: {
        ...(config.data ?? {}),
        message,
        type: 'warning',
        icon: 'question'
      },
    };

    const snackBarRef = this.snackBar.openFromComponent(QuestionPopupComponent, merged);
    return snackBarRef.afterDismissed().pipe<boolean>(
      map(res => res.dismissedByAction)
    );
  }
}