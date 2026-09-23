import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';

@Component({
  selector: 'crm-pretty-radio-button',
  standalone: false,
  templateUrl: './pretty-radio-button.component.html',
  styleUrl: './pretty-radio-button.component.css'
})
export class PrettyRadioButtonComponent implements OnInit {
  @Input() selected: boolean = false;
  @Input() selectHint: string = "Wybierz";
  @Input() unselectHint: string = "Odznacz";

  @Output() selectedChange = new EventEmitter<boolean>();

  constructor() {

  }

  ngOnInit(): void {
    
  }

  selectItem() {
    this.selected = !this.selected;
    this.selectedChange.emit(this.selected);
  }
}
