import { MatDialogConfig } from "@angular/material/dialog";

export class DialogConfig extends MatDialogConfig {
    constructor(data?: any, width: string = "100vw", height: string = "100vh") {
        super();
        this.width = width;
        this.height = height;
        this.data = data;
    }
}