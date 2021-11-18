export interface ILookupLoaderData {

}

export interface ILookupLoader extends ILookupLoaderData {
  isWorking: boolean;
  
  getLookupLabels(query: {
    LookupId: string;
    MenuId: string | undefined;
    LabelIds: string[];
  }): Promise<{ [key: string]: string }>;

  parent?: any
}