import 'package:client/auth.dart';
import 'package:flutter/material.dart';

abstract class AuthorizedView extends StatefulWidget {
  Account account;

  AuthorizedView(this.account);
}