import 'package:flutter/material.dart';
import 'package:viia_sample/screens/account_details_screen.dart';
import 'package:viia_sample/screens/accounts_screen.dart';
import 'package:viia_sample/screens/viia_login.dart';
import 'package:viia_sample/screens/welcome.dart';

void main() => runApp(MaterialApp(
  title: 'Viia Mobile Sample',
  initialRoute: '/',
  routes: <String, WidgetBuilder> {
    '/': (context) => WelcomeScreen(),
    '/login': (context) => ViiaLoginScreen(),
    '/accounts': (context) => AccountsScreen(),
    '/account-details': (context) => AccountDetailsScreen()
  }
));
