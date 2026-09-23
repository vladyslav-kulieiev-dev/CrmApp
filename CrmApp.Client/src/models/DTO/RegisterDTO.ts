import { UsersDTO } from "./UsersDTO";

export class RegisterDTO {
    userDTO?: UsersDTO = new UsersDTO();
    password?: string;
}