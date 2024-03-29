import 'package:client/auth.dart';
import 'package:client/data.dart';
import 'package:flutter/material.dart';

import 'chatsView.dart';
import 'loginView.dart';

void main() {
  runApp(const CrowMessenger());
}

class CrowMessenger extends StatelessWidget {
  const CrowMessenger({super.key});

  // This widget is the root of your application.
  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'Flutter Demo',
      theme: ThemeData(
        colorScheme: ColorScheme.fromSeed(seedColor: Colors.deepPurple),
        useMaterial3: true,
      ),
      home: LoginView()
    );
  }
}