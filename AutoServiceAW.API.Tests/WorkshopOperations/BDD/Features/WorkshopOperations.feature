Feature: Manage verifiable maintenance tasks
  As a workshop administrator or mechanic
  I want to register and update maintenance tasks
  So that I can track the actual progress of a service

  # Based on US-24 (EP-05).
  Scenario: Register a maintenance task for an active work order
    Given an active work order exists
    When an administrator registers a task with its description, mechanic, status, and estimated time
    Then the task is associated with the work order
    And the task is saved with its assigned mechanic and estimated time

  Scenario: Update a task's status and estimated time
    Given a maintenance task belongs to an active work order
    When an administrator or mechanic updates the task status and estimated time
    Then the task reflects the new status and estimated time
    And the task remains associated with the same work order
