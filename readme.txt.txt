1) ����� � ��� ���:
POST /token
������ ����� � ������ � ������ �����
(���� ����� � ������ ���� � ����)

GET /calculator/add/4/8
�������� ����������, ���������� ��� ��������
������ �����

GET /calculator/mult/3/5
�������� ����������, �������� ������ ���� ������� ������ �����
���������� ������������

2) ������� �������� ��� ��������:

��� 1 - �������� ������ � ������� � �������
=====================================
POST http://localhost:5000/token HTTP/1.1
Content-Type: application/x-www-form-urlencoded
Host: localhost:5000
Content-Length: 30

username=TEST&password=TEST123
=====================================

����� �������� � ����:
++++++++++++++++++++++++++++++++++++++
{
  "access_token": "eyJhbGciOiJI.......",
  "expires_in": 1800
}
++++++++++++++++++++++++++++++++++++++

��� 2 - ����� ������� ������ �� ���������, ��������� ���� �����
=========================
GET http://localhost:5000/api/calculator/mult/2/2 HTTP/1.1
Host: localhost:5000
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR.........
X-Requested-With: XMLHttpRequest

========================