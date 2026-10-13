Feature: Vehicle registration and fleet management

  As a workshop administrator
  I want to register and update customer vehicles in the fleet
  So that service orders and technical maintenance can be assigned and tracked accurately

  # Based on Fleet Management domain operations
  Scenario: Register a new vehicle for an existing customer
    Given a customer with ID 1 is registered in the workshop
    When an administrator registers a vehicle with plate "ABC-123", brand "Toyota", model "Corolla", year "2024", color "Black", and status "ACTIVE" for customer 1
    Then the vehicle is successfully registered in the fleet
    And the vehicle has status "ACTIVE"
    And the vehicle is associated with customer ID 1

  Scenario: Update existing vehicle operational status and specifications
    Given a vehicle with ID 1 and plate "ABC-123" exists in the fleet
    When an administrator updates the vehicle status to "IN_REPAIR" and color to "White"
    Then the vehicle reflects the updated status "IN_REPAIR"
    And the vehicle exterior color is updated to "White"
    And the vehicle remains associated with customer ID 1

  Scenario: Query vehicle information by unique identifier
    Given a vehicle with ID 1 and plate "ABC-123" is registered in the fleet
    When an authorized user requests the vehicle details for ID 1
    Then the vehicle details should be returned successfully
    And the response should contain the license plate "ABC-123"
    And the response should contain the brand "Toyota"
