import { ApiProperty } from '@nestjs/swagger';

export class CreateUserDto {
  @ApiProperty()
  readonly login: string;
  @ApiProperty()
  readonly password: string;
  @ApiProperty()
  readonly email?: string;
}
