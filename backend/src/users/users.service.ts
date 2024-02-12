import { HttpException, HttpStatus, Injectable } from '@nestjs/common';
import { Repository } from 'typeorm';
import { UserEntity } from './user.entity';
import { CreateUserDto } from './dto/create-user.dto';
import * as bcrypt from 'bcrypt';
import { InjectRepository } from '@nestjs/typeorm';

@Injectable()
export class UsersService {
  constructor(
    @InjectRepository(UserEntity)
    private readonly usersRepository: Repository<UserEntity>,
  ) {}

  async getAll(): Promise<CreateUserDto[]> {
    const db = this.usersRepository.createQueryBuilder();
    db.where('1 = 1');

    const users = await db.getMany();

    return users.map(
      (x) =>
        ({
          password: x.passwordHash,
          login: x.login,
          email: x.email,
        }) as CreateUserDto,
    );
  }

  async createAccount(model: CreateUserDto): Promise<void> {
    const user = await this.usersRepository.findOne({
      where: { login: model.login },
    });

    if (user) {
      const errors = { login: 'Login should be unique.' };
      throw new HttpException(
        { message: 'Input data validation failed', errors },
        HttpStatus.BAD_REQUEST,
      );
    }

    const saltOrRounds = 10;

    const newUser = new UserEntity();
    newUser.login = model.login;
    newUser.email = model.email;
    newUser.passwordHash = await bcrypt.hash(model.password, saltOrRounds);

    // TODO:
    // Add a user's validation =)

    await this.usersRepository.save(newUser);
  }
}
