Feature: Room Availability

# AC 4. Пошук доступних залів:
# - Вхідні дані: дата та час початку і завершення бронювання,
#   необхідна місткість.
# - Вихідні дані: список доступних залів.
Scenario: Search for available conference rooms
    Given a room with the following details
        | Name           | Capacity | HourlyRate |
        | Available Room | 50       | 2000       |
    When the client creates the room
    Then the response status code should be 201

    Given a room with the following details
        | Name           | Capacity | HourlyRate |
        | Too Small Room | 20       | 1500       |
    When the client creates the room
    Then the response status code should be 201

    When the client searches for available rooms
        | Start            | End              | Capacity |
        | 2026-10-01 10:00 | 2026-10-01 14:00 | 50       |

    Then the response status code should be 200
    And the following rooms should be returned
        | Name           |
        | Available Room |
