import { Body, Controller, Get, Post } from '@nestjs/common';
import { UsersService } from './users.service';
import { CreateUserDto } from './dto/create-user.dto';
import { ApiBearerAuth, ApiBody, ApiTags } from '@nestjs/swagger';

@ApiBearerAuth()
@ApiTags('users')
@Controller('users')
export class UsersController {
  constructor(private readonly userService: UsersService) {}

  @Post('users')
  @ApiBody({ type: CreateUserDto })
  async create(@Body() user: CreateUserDto) {
    return this.userService.createAccount(user);
  }

  @Get('users')
  async getAll() {
    return this.userService.getAll();
  }
}
