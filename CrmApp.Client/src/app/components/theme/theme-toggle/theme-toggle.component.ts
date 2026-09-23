import { Component, Input } from "@angular/core";
import { MatButtonToggleAppearance } from "@angular/material/button-toggle";
import { Theme, ThemeService } from "src/app/core/services/theme.service";

@Component({
    selector: 'crm-theme-toggle',
    standalone: false,
    templateUrl: './theme-toggle.component.html',
    styleUrls: ['./theme-toggle.component.css']
})
export class ThemeToggleComponent {     
    @Input() appearance: MatButtonToggleAppearance = 'standard';
    theme: Theme;

    constructor(public svc: ThemeService) {
        this.theme = this.svc.current;
    }

    onChange(v: Theme) { this.svc.setTheme(v); }
}