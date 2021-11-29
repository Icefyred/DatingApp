export interface Photo {
    id: number;
    url: string;
    isMain: boolean;
    /****** Phtoto Management Challenge 18. ******/
    isApproved: boolean;
    username?: string;
}