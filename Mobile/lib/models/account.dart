import 'dart:convert';

import 'package:viia_sample/models/resource.dart';

class Account {
  final String id;
  final String name;
  final String owner;
  final String type;
  final double available;
  final double booked;
  final String currency;
  final String number;

  Account({this.id, this.name, this.owner, this.type, this.available, this.booked, this.currency, this.number});

  factory Account.fromJson(Map<String,dynamic> json) {
    return Account(
      id: json['id'],
      name: json['name'],
      owner: json['owner'],
      type: json['type'],
      available: json['available'] != null ? json['available']['value'] : 0,
      booked: json['booked']['value'],
      currency: json['available'] != null ? json['available']['currency'] : json['booked'] != null ? json['booked']['currency'] : 'Unknown',
      number: json['number']['bban'] ?? json['number']['iban']
    );
  }

  static Resource<List<Account>> get all {
    return Resource(
      parse: (response) {
        final result = json.decode(response.body); 
        Iterable list = result['accounts'];
        return list.map((model) => Account.fromJson(model)).toList();
      }
    );
  }
}