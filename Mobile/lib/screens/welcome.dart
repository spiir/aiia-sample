import 'package:flutter/material.dart';
import '../services/viia_service.dart';


class WelcomeScreen extends StatefulWidget {
  @override
  State createState() => new WelcomeScreenState();
}

class WelcomeScreenState extends State<WelcomeScreen> {
  ViiaService _viiaService = new ViiaService();

  @override
  void initState() {
    super.initState();
    _viiaService.isAuthenticated().then((isAuthenticated) {
      setState(() {
              if(isAuthenticated) {
                Navigator.pushReplacementNamed(context, '/accounts');
              }
            });
    });
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Center( 
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          crossAxisAlignment: CrossAxisAlignment.center,
          children: <Widget>[
            Text(
              'Welcome to Viia Mobile Sample App'
            ),
            RaisedButton(
              onPressed: () {
                Navigator.pushReplacementNamed(context, '/login');
              },
              child: Text('Login via Viia')
            )
          ],
        ),
      ),
    );
  }
}