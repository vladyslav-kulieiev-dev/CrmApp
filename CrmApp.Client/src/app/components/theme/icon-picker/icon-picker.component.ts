import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { EmojiData } from '@ctrl/ngx-emoji-mart/ngx-emoji';

@Component({
  selector: 'crm-icon-picker',
  standalone: false,
  templateUrl: './icon-picker.component.html',
  styleUrl: './icon-picker.component.css'
})
export class IconPickerComponent implements OnInit {
  @Input() value: string = "memo";
  @Input() isReadonly: boolean = true;
  @Input() size: number = 20;
  @Input() buttonSize: "small" | "big" = "small";
  @Input() btnClass: string = "";

  @Output() valueChange: EventEmitter<string> = new EventEmitter<string>(); 

  valueStr: string = "memo";

  readonly translations = {
    search: 'Szukaj...',
    emojilist: 'Lista emoji',
    notfound: 'Nie znaleziono',
    clear: 'Wyczyść',
    categories: {
      search: 'Wyniki wyszukiwania',
      recent: 'Ostatnie',
      people: 'Reakcje | Ludzie',
      nature: 'Zwierzęta | Natura',
      foods: 'Jedzenie | Picie',
      activity: 'Aktywności',
      places: 'Podróże | Miejsca',
      objects: 'Obiekty',
      symbols: 'Symbole',
      flags: 'Flagi',
      custom: 'Własne',
    },
    skintones: {
      1: 'Domyślny',
      2: 'Jasny',
      3: 'Średnio Jasny',
      4: 'Średni',
      5: 'Średnio Ciemny',
      6: 'Ciemny',
    }
  }

  constructor() { }
  ngOnInit(): void {
    if (!this.value) this.value = "memo";
    this.valueStr = this.value;
  }

  emojiFallback = (emoji: any, props: any) => (emoji ? `:${emoji.shortNames[0]}:` : props.emoji);

  selectEmoji(event: any) {
    this.value = event.emoji.shortName;
    this.valueStr = event.emoji.shortName;
    this.valueChange.emit(this.value);
  }
}
