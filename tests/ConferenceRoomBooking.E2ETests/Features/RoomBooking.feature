Feature: Room booking

# AC 5. Бронювання залу:
# - Вхідні дані: ID залу, дата і час бронювання, тривалість, обрані послуги.
# - Вихідні дані: Підтвердження бронювання з розрахунком загальної вартості оренди.
Scenario: Book a conference room with additional services
    Given a room with the following details
        | Name         | Capacity | HourlyRate |
        | Meeting Room | 10       | 100        |
    And the room has the following services
        | Name      | Price |
        | Projector | 50    |
        | Catering  | 100   |
    When the client creates the room
    Then the response status code should be 201

    When the client books the room
        | Start            | End              | Services            |
        | 2026-10-01 10:00 | 2026-10-01 12:00 | Projector, Catering |

    Then the response status code should be 201
    And the booking should have total cost 350
    And the booking status should be "Confirmed"