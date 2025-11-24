export interface IWindowDescription {
  title: string;
  serviceFunction: (data: FormData) => Promise<any>;
}
