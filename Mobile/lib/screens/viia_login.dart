import 'dart:async';

import 'package:flutter/material.dart';
import 'package:webview_flutter/webview_flutter.dart';

import '../services/viia_service.dart';

class ViiaLoginScreen extends StatefulWidget {
  @override
  State createState() => ViiaLoginScreenState();
}

class ViiaLoginScreenState extends State<ViiaLoginScreen> {
  final Completer<WebViewController> _controller =
  Completer<WebViewController>();
  ViiaService _viiaService = ViiaService();

  @override
  Widget build(BuildContext context) {
    return Scaffold(
        appBar: AppBar(
          title: const Text("Viia Connect")
        ),
         body: Builder(builder: (BuildContext context) {
        return WebView(
          initialUrl: _viiaService.getConnectUrl(),
          javascriptMode: JavascriptMode.unrestricted,
          onWebViewCreated: (WebViewController webViewController) {
            _controller.complete(webViewController);
          },
          navigationDelegate: (NavigationRequest request) {
            if (request.url.contains('?code=')) {
              var uri = Uri.parse(request.url);
              var code = uri.queryParameters['code'];
              _viiaService.fetchAccessToken(code).then((_) {
                Navigator.pushReplacementNamed(context, '/accounts');
              });
            }
            return NavigationDecision.navigate;
          },
          onPageFinished: (String url) {
            print('Page finished loading: $url');
          },
        );
      }),
    );
  }
}