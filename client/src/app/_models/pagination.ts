export interface Pagination{
    currentPage: number;
    itemsPerPage: number;
    totalItems: number;
    totalPages: number;
}

export class PaginatedResult<T>{
    //for the initial example T will represent an array of members: Member[]
    result: T;
    pagination: Pagination;
}