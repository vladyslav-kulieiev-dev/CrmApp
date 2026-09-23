import { Component, Input } from '@angular/core';
import { DictionariesElements } from 'src/models/DictionariesElements';
import { TaskListItemDTO } from 'src/models/DTO/tasks/TaskListItemDTO';

const BADGE_COLOR_CYCLE = ['badge-todo', 'badge-progress', 'badge-done', 'badge-urgent', 'badge-normal', 'badge-low'];
const PRIORITY_COLORS   = ['badge-urgent', 'badge-normal', 'badge-low'];

@Component({
  selector: 'crm-tasks-stats',
  standalone: false,
  templateUrl: './tasks-stats.component.html',
  styleUrl: './tasks-stats.component.css',
})
export class TasksStatsComponent {
  @Input() tasks:            TaskListItemDTO[]      = [];
  @Input() total:            number                 = 0;
  @Input() tasksStates:      DictionariesElements[] = [];
  @Input() tasksPriorities:  DictionariesElements[] = [];
  @Input() showWorkload = true;


  private get doneValue(): string {
    return this.tasksStates.at(-1)?.value ?? '';
  }

  get summary() {
    const now = new Date();
    const dv  = this.doneValue;
    const t   = this.tasks;
    return {
      total:       this.total,
      done:        t.filter(x => x.state === dv).length,
      inProgress:  t.filter(x => {
                     const idx = this.tasksStates.findIndex(s => s.value === x.state);
                     return idx > 0 && idx < this.tasksStates.length - 1;
                   }).length,
      overdue:     t.filter(x => x.dueDate && new Date(x.dueDate as any) < now && x.state !== dv).length,
      unassigned:  t.filter(x => !x.assignedTo).length,
      avgProgress: t.length
        ? Math.round(t.reduce((s, x) => s + (x.progress ?? 0), 0) / t.length)
        : 0,
    };
  }

  get byStatus() {
    const n = this.tasks.length || 1;
    return this.tasksStates.map((s, i) => {
      const count = this.tasks.filter(t => t.state === s.value).length;
      return {
        label:    s.key,
        count,
        percent:  Math.round(count / n * 100),
        cssClass: BADGE_COLOR_CYCLE[i % BADGE_COLOR_CYCLE.length],
      };
    });
  }

  get byPriority() {
    const n = this.tasks.length || 1;
    return this.tasksPriorities.map((p, i) => {
      const count = this.tasks.filter(t => t.priority === p.value).length;
      return {
        label:    p.key,
        count,
        percent:  Math.round(count / n * 100),
        cssClass: PRIORITY_COLORS[i % PRIORITY_COLORS.length],
      };
    });
  }

  get workload() {
    const dv  = this.doneValue;
    const now = new Date();
    const map = new Map<number, { name: string; total: number; overdue: number }>();
    this.tasks.forEach(t => {
      if (!t.assignedTo) return;
      const e = map.get(t.assignedTo) ?? { name: t.assignedToFullName ?? '', total: 0, overdue: 0 };
      e.total++;
      if (t.dueDate && new Date(t.dueDate as any) < now && t.state !== dv) e.overdue++;
      map.set(t.assignedTo, e);
    });
    const max = Math.max(...[...map.values()].map(v => v.total), 1);
    return [...map.entries()]
      .map(([id, v]) => ({
        userId:          id,
        fullName:        v.name,
        initials:        v.name.split(' ').map((n: string) => n[0]).join('').slice(0, 2).toUpperCase(),
        totalTasks:      v.total,
        overdueTasks:    v.overdue,
        workloadPercent: Math.round((v.total / max) * 100),
      }))
      .sort((a, b) => b.totalTasks - a.totalTasks);
  }
}