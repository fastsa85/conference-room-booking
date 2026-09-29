Feature: Room management

# AC 1. Додавання конференц-залу:
#  - Вхідні дані: Назва залу (наприклад, "Зал А"), місткість (наприклад, 50 осіб), список 
#    доступних послуг (наприклад, проєктор, вартість 500 гривень; Wi-Fi, вартість 300 
#    гривень), базова вартість оренди за годину (наприклад, 2000 гривень).
#  - Вихідні дані: Підтвердження успішного створення залу з унікальним ID.
Scenario: Create a conference room with additional services
    Given a room with the following details
        | Name           | Capacity | HourlyRate |
        | Meeting Room A | 50       | 2000       |
    And the room has the following services
        | Name      | Price |
        | Projector | 500   |
        | Wi-Fi     | 300   |

    When the client creates the room
    Then the response status code should be 201
    And the room should have the following details
        | Name           | Capacity | HourlyRate |
        | Meeting Room A | 50       | 2000       |
    And the room should have the following services
        | Name      | Price |
        | Projector | 500   |
        | Wi-Fi     | 300   |

    When the client requests the room
    Then the response status code should be 200
    And the room should have the following details
        | Name           | Capacity | HourlyRate |
        | Meeting Room A | 50       | 2000       |
    And the room should have the following services
        | Name      | Price |
        | Projector | 500   |
        | Wi-Fi     | 300   |


# AC 2. Редагування інформації про зал:
# - Вхідні дані: ID залу, оновлені дані (наприклад, зміна вартості оренди до 2500
#   гривень або додавання послуги "Звук", вартість 700 гривень).
# - Вихідні дані: Підтвердження успішного оновлення.
Scenario: Update conference room information
    Given a room with the following details
        | Name           | Capacity | HourlyRate |
        | Meeting Room A | 50       | 2000       |
    And the room has the following services
        | Name      | Price |
        | Projector | 500   |
        | Wi-Fi     | 300   |
    When the client creates the room
    Then the response status code should be 201

    Given a room with the following details
        | Name           | Capacity | HourlyRate |
        | Meeting Room A | 50       | 2500       |
    And the room has the following services
        | Name      | Price |
        | Projector | 500   |
        | Wi-Fi     | 300   |
        | Sound     | 700   |
    When the client updates the room
    Then the response status code should be 204

    When the client requests the room
    Then the response status code should be 200
    And the room should have the following details
        | Name           | Capacity | HourlyRate |
        | Meeting Room A | 50       | 2500       |
    And the room should have the following services
        | Name      | Price |
        | Projector | 500   |
        | Wi-Fi     | 300   |
        | Sound     | 700   |


# AC 3. Видалення конференц-залу:
# - Вхідні дані: ID залу.
# - Вихідні дані: Підтвердження успішного видалення залу.
Scenario: Delete a conference room
    Given a room with the following details
        | Name           | Capacity | HourlyRate |
        | Meeting Room A | 50       | 2000       |
    And the room has the following services
        | Name      | Price |
        | Projector | 500   |
        | Wi-Fi     | 300   |
    When the client creates the room
    Then the response status code should be 201

    When the client deletes the room
    Then the response status code should be 204

    When the client requests the room
    Then the response status code should be 404


Scenario: Deleting a room does not affect other rooms and their services
    Given a room with the following details
        | Name           | Capacity | HourlyRate |
        | Meeting Room A | 50       | 2000       |
    And the room has the following services
        | Name      | Price |
        | Projector | 500   |
        | Wi-Fi     | 300   |
    When the client creates the room
    Then the response status code should be 201

    Given a room with the following details
        | Name           | Capacity | HourlyRate |
        | Meeting Room B | 20       | 1500       |
    And the room has the following services
        | Name      | Price |
        | Projector | 500   |
        | Wi-Fi     | 300   |
    When the client creates the room
    Then the response status code should be 201

    When the client deletes the room
    Then the response status code should be 204

    When the client requests all rooms
    Then the response status code should be 200
    And the following rooms should be returned
        | Name           |
        | Meeting Room A |

    When the client requests the room "Meeting Room A"
    Then the response status code should be 200
    And the room should have the following services
        | Name      | Price |
        | Projector | 500   |
        | Wi-Fi     | 300   |