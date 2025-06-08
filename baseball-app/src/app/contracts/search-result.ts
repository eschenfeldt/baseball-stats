export enum SearchResultType {
    player,
    team
}
export interface SearchResult {
    id: string;
    name: string;
    type: SearchResultType;
    description: string;
}