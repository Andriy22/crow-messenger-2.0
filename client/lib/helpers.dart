import 'package:intl/intl.dart';

var minutesFormat = NumberFormat("00", "en_US");

String getShortDate(DateTime time) {
  return "${time.hour}:${minutesFormat.format(time.minute)}";
}