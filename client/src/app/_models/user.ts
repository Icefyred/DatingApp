//interfaces in typescript are a bit different to interfaces in C#,
//when used in ts is to specify that it's a type of something
export interface User{
    username: string;
    token: string;
    photoUrl: string;
}
/*
//allows in the initialization of 'data' to be of type number or string, so in the process of coding allow to be
//more flexible to the type of value that will be necessary at the time.
let data: number | string = 42;

data = "10";

interface Car{
    color: string;
    model: string;
    //topSpeed? means that this attribute is optional
    topSpeed?: number;
}

const car1: Car = {
    color: 'blue',
    model: 'BMW'
}

const car2: Car = {
    color: 'red',
    model: 'Mercedes',
    topSpeed: 100
}

const multiply = (x: number, y: number): void => {
    x * y;
}
*/