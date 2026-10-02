Feature: Authentication and role-based access

  As an AutoService user
  I want to authenticate with my registered credentials
  So that I can access the application according to my assigned role

  Scenario: Administrator signs in with valid credentials
    Given an administrator with email "admin@autoservice.com" is registered
    And the administrator has password "SecurePassword123"
    When the administrator signs in with valid credentials
    Then the authentication request should be successful
    And the response should contain the role "admin"
    And the response should contain a JWT access token
    And the administrator should have access to the administration area

  Scenario: User signs in with invalid credentials
    Given a user with email "admin@autoservice.com" is registered
    When the user signs in with an incorrect password
    Then the authentication request should be rejected
    And the response should not contain a JWT access token