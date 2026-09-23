import { Directive, Input, TemplateRef, ViewContainerRef } from '@angular/core';

@Directive({ 
  selector: '[crmTemplate]'
})
export class CrmTemplateDirective {
  @Input('crmTemplate') slot!: string; 
  constructor(public templateRef: TemplateRef<unknown>, vcr: ViewContainerRef) {}
}
