export class TreeNode {
    data?: object;
    children?: TreeNode[];
    expanded: boolean = false;
    selected: boolean = false;
    level: number = 0;
    parent?: TreeNode;
    id?: number;
    label: string = '';
    icon?: string;
    createdAt: Date = new Date();
}